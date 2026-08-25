import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';

import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatDialogModule } from '@angular/material/dialog';

import { finalize } from 'rxjs';

import { ProductService } from '../../../core/services/product.service';
import { Product } from '../../../shared/models/product';
import { MatDialog } from '@angular/material/dialog';
import { ProductForm } from '../product-form/product-form';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatTableModule,
    MatDialogModule,
  ],
  templateUrl: './product-list.html',
  styleUrl: './product-list.scss',
})
export class ProductList implements OnInit {
  products: Product[] = [];

  loading = false;

  errorMessage = '';

  readonly displayedColumns = ['code', 'description', 'stockQuantity'];

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
          this.errorMessage = 'Não foi possível carregar os produtos.';
        },
      });
  }

  openCreateDialog(): void {
    const dialogRef = this.dialog.open(ProductForm, {
      width: '560px',
      maxWidth: '95vw',
    });

    dialogRef.afterClosed().subscribe((created) => {
      if (created) {
        this.loadProducts();
      }
    });
  }
}
