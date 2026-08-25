import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectorRef, Component, Inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { finalize } from 'rxjs';

import { InvoiceService } from '../../../core/services/invoice.service';
import { Invoice } from '../../../shared/models/invoice';

@Component({
  selector: 'app-invoice-print-confirm',
  standalone: true,
  imports: [MatButtonModule, MatDialogModule, MatProgressSpinnerModule],
  templateUrl: './invoice-print-confirm.html',
  styleUrl: './invoice-print-confirm.scss',
})
export class InvoicePrintConfirm {
  printing = false;

  constructor(
    @Inject(MAT_DIALOG_DATA)
    readonly invoice: Invoice,
    private readonly invoiceService: InvoiceService,
    private readonly dialogRef: MatDialogRef<InvoicePrintConfirm>,
    private readonly snackBar: MatSnackBar,
    private readonly changeDetectorRef: ChangeDetectorRef,
  ) {}

  confirm(): void {
    if (this.printing || this.invoice.status !== 'Open') {
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
        next: () => {
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

  formatNumber(number: number): string {
    return number.toString().padStart(6, '0');
  }
}
