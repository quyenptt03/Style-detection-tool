import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: false,
  styleUrl: './app.component.css',
})
export class AppComponent {
  title = 'client-app';
  form: FormGroup;
  selectedFile: File | null = null;

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      url: ['', [Validators.required, Validators.pattern('https?://.+')]],
    });
  }

  onFileChange(event: any) {
    const file: File = event.target.files[0];
    if (
      file &&
      (file.type === 'application/json' || file.type === 'text/plain')
    ) {
      this.selectedFile = file;
    } else {
      this.selectedFile = null;
      alert('Chỉ chấp nhận file .txt hoặc .json');
    }
  }

  onSubmit() {
    if (this.form.valid && this.selectedFile) {
      const url = this.form.value.url;
      const file = this.selectedFile;

      console.log('URL:', url);
      console.log('File:', file.name);

      const reader = new FileReader();
      reader.onload = () => {
        console.log('Nội dung file:', reader.result);
      };
      reader.readAsText(file);
    } else {
      alert('Vui lòng nhập URL hợp lệ và chọn file.');
    }
  }
}
