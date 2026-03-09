import { Component, ElementRef } from '@angular/core';
import { NgForm } from '@angular/forms';

@Component({
  selector: 'app-purchase',
  standalone: false,
  templateUrl: './purchase.html',
  styleUrl: './purchase.scss',
})
export class Purchase {
  selectedFile!: File;
  submitUploadStatus(form: NgForm) {
    alert(form.value.statusFile);
  }
  submitPurchase(form: NgForm) {

    if (!this.selectedFile) {
      console.log("No file selected");
      return;
    }

    const formData = new FormData();
    formData.append('file', this.selectedFile);

    alert(this.selectedFile.name);
  }

  onFileSelected(event: any) {
    const input = event.target as HTMLInputElement;

    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
    }
  }
}