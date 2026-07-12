import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { AutoCompleteCompleteEvent, AutoCompleteModule } from 'primeng/autocomplete';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';
import { InputNumberModule } from 'primeng/inputnumber';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { startWith } from 'rxjs';
import { LocationDetail } from '../../core/models/location.model';
import { PurchaseItem } from '../../core/models/purchase-item.model';
import { AuthService } from '../../core/services/auth.service';
import { LocationService } from '../../core/services/location.service';
import { PurchaseBillService } from '../../core/services/purchase-bill.service';
import { AppShellComponent } from '../../layout/app-shell.component';
import { getValidationMessage } from '../../shared/validators/form-error.util';

const itemOptions = ['Mango', 'Apple', 'Banana', 'Orange', 'Grapes', 'Kiwi', 'Strawberry'];

@Component({
  selector: 'app-purchase-bill',
  standalone: true,
  imports: [
    AppShellComponent,
    AutoCompleteModule,
    ButtonModule,
    CardModule,
    DecimalPipe,
    DividerModule,
    InputNumberModule,
    ProgressSpinnerModule,
    ReactiveFormsModule,
    SelectModule,
    TableModule,
    TagModule
  ],
  templateUrl: './purchase-bill.component.html'
})
export class PurchaseBillComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly locationService = inject(LocationService);
  private readonly purchaseBillService = inject(PurchaseBillService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly messageService = inject(MessageService);

  readonly items = signal<PurchaseItem[]>([]);
  readonly locations = signal<LocationDetail[]>([]);
  readonly filteredItems = signal<string[]>(itemOptions);
  readonly isLoadingLocations = signal(false);
  readonly isSaving = signal(false);
  private readonly formVersion = signal(0);

  readonly totalItems = computed(() => this.items().length);
  readonly totalQuantity = computed(() => this.items().reduce((sum, item) => sum + item.quantity, 0));
  readonly totalCost = computed(() => this.items().reduce((sum, item) => sum + item.totalCost, 0));
  readonly totalSelling = computed(() => this.items().reduce((sum, item) => sum + item.totalSelling, 0));
  readonly previewTotalCost = computed(() => this.calculateTotalCost());
  readonly previewTotalSelling = computed(() => this.calculateTotalSelling());

  readonly form = this.formBuilder.nonNullable.group({
    item: ['', Validators.required],
    batch: ['', Validators.required],
    standardCost: [0, [Validators.required, Validators.min(0.01)]],
    standardPrice: [0, [Validators.required, Validators.min(0.01)]],
    quantity: [1, [Validators.required, Validators.min(1)]],
    discount: [0, [Validators.required, Validators.min(0), Validators.max(100)]]
  });

  ngOnInit(): void {
    this.form.valueChanges.pipe(startWith(this.form.getRawValue())).subscribe(() => {
      this.formVersion.update(version => version + 1);
    });

    this.loadLocations();
  }

  filterItems(event: AutoCompleteCompleteEvent): void {
    const query = event.query.toLowerCase();
    this.filteredItems.set(itemOptions.filter(item => item.toLowerCase().includes(query)));
  }

  validationMessage(controlName: keyof typeof this.form.controls, label: string): string {
    return getValidationMessage(this.form.controls[controlName], label);
  }

  addItem(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const purchaseItem: PurchaseItem = {
      item: value.item,
      batch: value.batch,
      standardCost: value.standardCost,
      standardPrice: value.standardPrice,
      quantity: value.quantity,
      discount: value.discount,
      totalCost: this.calculateTotalCost(),
      totalSelling: this.calculateTotalSelling()
    };

    this.items.update(items => [...items, purchaseItem]);
    this.resetForm();
    this.messageService.add({ severity: 'success', summary: 'Item added', detail: `${purchaseItem.item} added to the bill.` });
  }

  removeItem(index: number): void {
    this.items.update(items => items.filter((_, currentIndex) => currentIndex !== index));
    this.messageService.add({ severity: 'info', summary: 'Item removed', detail: 'The purchase summary has been updated.' });
  }

  saveBill(): void {
    if (this.items().length === 0 || this.isSaving()) {
      return;
    }

    this.isSaving.set(true);
    this.purchaseBillService.save({ items: this.items() }).subscribe({
      next: response => {
        this.messageService.add({ severity: 'success', summary: 'Bill saved', detail: `Purchase bill #${response.id} saved successfully.` });
      },
      error: (error: HttpErrorResponse) => {
        this.messageService.add({ severity: 'error', summary: 'Save failed', detail: error.error?.message ?? 'Please try again.' });
      },
      complete: () => this.isSaving.set(false)
    });
  }

  logout(): void {
    this.authService.logout();
    void this.router.navigate(['/login']);
  }

  private loadLocations(): void {
    this.isLoadingLocations.set(true);
    this.locationService.getLocations().subscribe({
      next: locations => this.locations.set(locations),
      error: (error: HttpErrorResponse) => {
        this.isLoadingLocations.set(false);
        this.messageService.add({ severity: 'error', summary: 'Unable to load batches', detail: error.error?.message ?? 'Please login again.' });
        if (error.status === 401) {
          this.logout();
        }
      },
      complete: () => this.isLoadingLocations.set(false)
    });
  }

  private resetForm(): void {
    this.form.reset({
      item: '',
      batch: '',
      standardCost: 0,
      standardPrice: 0,
      quantity: 1,
      discount: 0
    });
  }

  private calculateTotalCost(): number {
    // Signals only recalculate when signal reads change; this read links the preview to form changes.
    this.formVersion();
    const { standardCost, quantity, discount } = this.form.getRawValue();
    const grossCost = standardCost * quantity;
    return grossCost - grossCost * (discount / 100);
  }

  private calculateTotalSelling(): number {
    this.formVersion();
    const { standardPrice, quantity } = this.form.getRawValue();
    return standardPrice * quantity;
  }
}
