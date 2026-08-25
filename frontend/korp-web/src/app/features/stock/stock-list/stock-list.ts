import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { finalize } from 'rxjs';

import { ProductService } from '../../../core/services/product.service';
import { Product } from '../../../shared/models/product';
import { StockIncrease } from '../../products/stock-increase/stock-increase';

@Component({
  selector: 'app-stock-list',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatCardModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    MatTableModule,
  ],
  templateUrl: './stock-list.html',
  styleUrl: './stock-list.scss',
})
export class StockList implements OnInit {
  products: Product[] = [];
  loading = false;
  errorMessage = '';

  readonly displayedColumns = ['code', 'description', 'stockQuantity', 'entry'];

  constructor(
    private readonly productService: ProductService,
    private readonly dialog: MatDialog,
    private readonly changeDetectorRef: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.loading = true;
    this.errorMessage = '';

    this.productService
      .getAll()
      .pipe(
        finalize(() => {
          this.loading = false;
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: (products) => {
          this.products = products;
        },
        error: () => {
          this.errorMessage = 'Não foi possível carregar os saldos em estoque.';
        },
      });
  }

  openStockDialog(product: Product): void {
    const dialogRef = this.dialog.open(StockIncrease, {
      width: '480px',
      maxWidth: '95vw',
      data: product,
    });

    dialogRef.afterClosed().subscribe((updated) => {
      if (updated) {
        this.loadProducts();
      }
    });
  }
}
