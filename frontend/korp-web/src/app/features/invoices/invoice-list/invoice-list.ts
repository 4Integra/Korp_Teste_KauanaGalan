import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, ViewChild } from '@angular/core';

import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';

import { finalize } from 'rxjs';

import { InvoiceService } from '../../../core/services/invoice.service';
import { Invoice } from '../../../shared/models/invoice';
import { InvoiceCreate } from '../invoice-create/invoice-create';
import { InvoiceDetails } from '../invoice-details/invoice-details';
import { InvoicePrintConfirm } from '../invoice-print-confirm/invoice-print-confirm';

@Component({
  selector: 'app-invoice-list',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatCardModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    MatSortModule,
    MatTableModule,
  ],
  templateUrl: './invoice-list.html',
  styleUrl: './invoice-list.scss',
})
export class InvoiceList implements OnInit {
  readonly dataSource = new MatTableDataSource<Invoice>([]);

  loading = false;

  errorMessage = '';

  readonly displayedColumns = ['number', 'createdAt', 'items', 'status', 'actions'];

  @ViewChild(MatSort)
  set sort(sort: MatSort | undefined) {
    if (sort) {
      this.dataSource.sort = sort;
    }
  }

  constructor(
    private readonly invoiceService: InvoiceService,
    private readonly dialog: MatDialog,
    private readonly changeDetectorRef: ChangeDetectorRef,
  ) {
    this.dataSource.sortingDataAccessor = (invoice: Invoice, column: string): string | number => {
      switch (column) {
        case 'createdAt':
          return Date.parse(invoice.createdAt);
        case 'items':
          return invoice.items.length;
        case 'status':
          return invoice.status === 'Open' ? 0 : 1;
        default:
          return invoice.number;
      }
    };
  }

  ngOnInit(): void {
    this.loadInvoices();
  }

  loadInvoices(): void {
    this.loading = true;
    this.errorMessage = '';

    this.invoiceService
      .getAll()
      .pipe(
        finalize(() => {
          this.loading = false;
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: (invoices) => {
          this.dataSource.data = invoices;
        },

        error: () => {
          this.errorMessage = 'Não foi possível carregar as notas fiscais.';
        },
      });
  }

  formatNumber(number: number): string {
    return number.toString().padStart(6, '0');
  }

  getStatusLabel(status: string): string {
    return status === 'Open' ? 'Aberta' : 'Fechada';
  }

  isOpen(invoice: Invoice): boolean {
    return invoice.status === 'Open';
  }

  openCreateDialog(): void {
    const dialogRef = this.dialog.open(InvoiceCreate, {
      width: '920px',
      maxWidth: '96vw',
      disableClose: true,
    });

    dialogRef.afterClosed().subscribe((created) => {
      if (created) {
        this.loadInvoices();
      }
    });
  }

  openDetailsDialog(invoice: Invoice): void {
    const dialogRef = this.dialog.open(InvoiceDetails, {
      width: '760px',
      maxWidth: '96vw',
      data: {
        invoiceId: invoice.id,
      },
    });

    dialogRef.afterClosed().subscribe((changed) => {
      if (changed) {
        this.loadInvoices();
      }
    });
  }

  openPrintDialog(invoice: Invoice): void {
    if (!this.isOpen(invoice)) {
      return;
    }

    const dialogRef = this.dialog.open(InvoicePrintConfirm, {
      width: '480px',
      maxWidth: '95vw',
      disableClose: true,
      data: invoice,
    });

    dialogRef.afterClosed().subscribe((printed) => {
      if (printed) {
        this.loadInvoices();
      }
    });
  }
}
