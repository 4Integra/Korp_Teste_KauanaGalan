import { ChangeDetectorRef, Component } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';

import { finalize } from 'rxjs';

import { ProductService } from '../../../core/services/product.service';

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './product-form.html',
  styleUrl: './product-form.scss',
})
export class ProductForm {
  submitting = false;
  submitted = false;

  readonly form;

  constructor(
    private readonly formBuilder: FormBuilder,
    private readonly productService: ProductService,
    private readonly dialogRef: MatDialogRef<ProductForm>,
    private readonly snackBar: MatSnackBar,
    private readonly changeDetectorRef: ChangeDetectorRef,
  ) {
    this.form = this.formBuilder.nonNullable.group({
      code: ['', [Validators.required, Validators.maxLength(50)]],
      description: ['', [Validators.required, Validators.maxLength(200)]],
      stockQuantity: [0, [Validators.required, Validators.min(0)]],
    });
  }

  submit(): void {
    this.submitted = true;

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting = true;

    this.productService
      .create(this.form.getRawValue())
      .pipe(
        finalize(() => {
          this.submitting = false;
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: () => {
          this.snackBar.open('Produto cadastrado com sucesso.', 'Fechar', {
            duration: 3000,
            panelClass: ['success-snackbar'],
          });

          this.dialogRef.close(true);
        },

        error: (error: HttpErrorResponse) => {
          const message = error.error?.detail ?? 'Não foi possível cadastrar o produto.';

          this.snackBar.open(message, 'Fechar', {
            duration: 5000,
          });
        },
      });
  }
}
