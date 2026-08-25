import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectorRef, Component, Inject, OnInit } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { catchError, finalize, forkJoin, of } from 'rxjs';

import { InvoiceService } from '../../../core/services/invoice.service';
import { ProductService } from '../../../core/services/product.service';
import { Invoice } from '../../../shared/models/invoice';
import { Product } from '../../../shared/models/product';

export interface InvoiceDetailsData {
  invoiceId: string;
}

@Component({
  selector: 'app-invoice-details',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    MatTableModule,
  ],
  templateUrl: './invoice-details.html',
  styleUrl: './invoice-details.scss',
})
export class InvoiceDetails implements OnInit {
  invoice?: Invoice;
  productsById = new Map<string, Product>();
  loading = false;
  printing = false;
  errorMessage = '';
  changed = false;

  readonly displayedColumns = ['product', 'quantity'];

  constructor(
    @Inject(MAT_DIALOG_DATA)
    readonly data: InvoiceDetailsData,
    private readonly invoiceService: InvoiceService,
    private readonly productService: ProductService,
    private readonly dialogRef: MatDialogRef<InvoiceDetails>,
    private readonly snackBar: MatSnackBar,
    private readonly changeDetectorRef: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.loadDetails();
  }

  loadDetails(): void {
    this.loading = true;
    this.errorMessage = '';

    forkJoin({
      invoice: this.invoiceService.getById(this.data.invoiceId),
      products: this.productService.getAll().pipe(catchError(() => of([] as Product[]))),
    })
      .pipe(
        finalize(() => {
          this.loading = false;
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: ({ invoice, products }) => {
          this.invoice = invoice;
          this.productsById = new Map(products.map((product) => [product.id, product]));
        },
        error: () => {
          this.errorMessage = 'Não foi possível carregar os detalhes da nota fiscal.';
        },
      });
  }

  printInvoice(): void {
    if (!this.invoice || this.invoice.status !== 'Open') {
      return;
    }

    this.printing = true;

    this.invoiceService
      .print(this.invoice.id)
      .pipe(
        finalize(() => {
          this.printing = false;
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: (invoice) => {
          this.invoice = invoice;
          this.changed = true;

          this.snackBar.open('Nota impressa e estoque atualizado com sucesso.', 'Fechar', {
            duration: 4000,
            panelClass: ['success-snackbar'],
          });

          this.dialogRef.close(true);
        },
        error: (error: HttpErrorResponse) => {
          this.snackBar.open(
            error.error?.detail ?? 'Não foi possível imprimir a nota. Tente novamente.',
            'Fechar',
            {
              duration: 6000,
              panelClass: ['error-snackbar'],
              verticalPosition: 'top',
            },
          );
        },
      });
  }

  close(): void {
    this.dialogRef.close(this.changed);
  }

  formatNumber(number: number): string {
    return number.toString().padStart(6, '0');
  }

  getProductCode(productId: string): string {
    return this.productsById.get(productId)?.code ?? 'Produto';
  }

  getProductDescription(productId: string): string {
    return this.productsById.get(productId)?.description ?? productId;
  }
}
