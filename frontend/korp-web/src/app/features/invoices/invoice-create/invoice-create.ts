import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { finalize } from 'rxjs';

import { InvoiceService } from '../../../core/services/invoice.service';
import { ProductService } from '../../../core/services/product.service';
import { Product } from '../../../shared/models/product';

@Component({
  selector: 'app-invoice-create',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
  ],
  templateUrl: './invoice-create.html',
  styleUrl: './invoice-create.scss',
})
export class InvoiceCreate implements OnInit {
  products: Product[] = [];
  loadingProducts = false;
  submitting = false;
  errorMessage = '';

  readonly form;

  constructor(
    private readonly formBuilder: FormBuilder,
    private readonly invoiceService: InvoiceService,
    private readonly productService: ProductService,
    private readonly dialogRef: MatDialogRef<InvoiceCreate>,
    private readonly snackBar: MatSnackBar,
    private readonly changeDetectorRef: ChangeDetectorRef,
  ) {
    this.form = this.formBuilder.nonNullable.group({
      items: this.formBuilder.array([this.createItemGroup()]),
    });
  }

  ngOnInit(): void {
    this.loadProducts();
  }

  get items(): FormArray {
    return this.form.controls.items;
  }

  loadProducts(): void {
    this.loadingProducts = true;
    this.errorMessage = '';

    this.productService
      .getAll()
      .pipe(
        finalize(() => {
          this.loadingProducts = false;
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: (products) => {
          this.products = products;
        },
        error: () => {
          this.errorMessage = 'Não foi possível carregar os produtos do estoque.';
        },
      });
  }

  addItem(): void {
    this.items.push(this.createItemGroup());
  }

  removeItem(index: number): void {
    if (this.items.length > 1) {
      this.items.removeAt(index);
    }
  }

  isProductSelected(productId: string, currentIndex: number): boolean {
    return this.items.controls.some(
      (control, index) => index !== currentIndex && control.get('productId')?.value === productId,
    );
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting = true;

    this.invoiceService
      .create(this.form.getRawValue())
      .pipe(
        finalize(() => {
          this.submitting = false;
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: (invoice) => {
          this.snackBar.open(
            `Nota #${invoice.number.toString().padStart(6, '0')} criada com sucesso.`,
            'Fechar',
            {
              duration: 3500,
              panelClass: ['success-snackbar'],
            },
          );

          this.dialogRef.close(invoice);
        },
        error: (error: HttpErrorResponse) => {
          this.snackBar.open(
            error.error?.detail ?? 'Não foi possível criar a nota fiscal.',
            'Fechar',
            { duration: 5000 },
          );
        },
      });
  }

  private createItemGroup() {
    return this.formBuilder.nonNullable.group({
      productId: ['', Validators.required],
      quantity: [1, [Validators.required, Validators.min(1)]],
    });
  }
}
