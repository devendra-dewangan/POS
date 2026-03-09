import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ImportRoutingModule } from './import-routing-module';
import { Purchase } from './pages/purchase/purchase';
import { Sale } from './pages/sale/sale';
import { Product } from './pages/product/product';
import { Buyer } from './pages/buyer/buyer';
import { Saller } from './pages/saller/saller';
import { FormsModule } from '@angular/forms';

@NgModule({
  declarations: [Purchase, Sale, Product, Buyer, Saller],
  imports: [CommonModule, ImportRoutingModule, FormsModule],
})
export class ImportModule {}
