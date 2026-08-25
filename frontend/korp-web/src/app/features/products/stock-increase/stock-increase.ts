import { ChangeDetectorRef, Component, Inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';

import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';

import { finalize } from 'rxjs';

import { ProductService } from '../../../core/services/product.service';
import { Product } from '../../../shared/models/product';

@Component({
  selector: 'app-stock-increase',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './stock-increase.html',
  styleUrl: './stock-increase.scss',
})
export class StockIncrease {
  submitting = false;

  readonly form;

  constructor(
    @Inject(MAT_DIALOG_DATA)
    readonly product: Product,

    private readonly formBuilder: FormBuilder,
    private readonly productService: ProductService,
    private readonly dialogRef: MatDialogRef<StockIncrease>,
    private readonly snackBar: MatSnackBar,
    private readonly changeDetectorRef: ChangeDetectorRef,
  ) {
    this.form = this.formBuilder.nonNullable.group({
      quantity: [1, [Validators.required, Validators.min(1)]],
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting = true;

    this.productService
      .increaseStock({
        items: [
          {
            productId: this.product.id,
            quantity: this.form.controls.quantity.value,
          },
        ],
      })
      .pipe(
        finalize(() => {
          this.submitting = false;
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: (result) => {
          this.snackBar.open(result.message, 'Fechar', {
            duration: 3000,
            panelClass: ['success-snackbar'],
          });

          this.dialogRef.close(true);
        },

        error: (error: HttpErrorResponse) => {
          this.snackBar.open(
            error.error?.detail ?? 'Não foi possível adicionar o estoque.',
            'Fechar',
            {
              duration: 5000,
            },
          );
        },
      });
  }
}
