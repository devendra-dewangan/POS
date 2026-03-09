import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Purchase } from './pages/purchase/purchase';

const routes: Routes = [
  {
    path: '',
    component:Purchase
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ImportRoutingModule {}
