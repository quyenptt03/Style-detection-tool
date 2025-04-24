import { Component, ViewChild, ElementRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

interface PageInfo {
  url: string;
  title: string;
  section: string[];
}

interface Style {
  name: string;
  isUsed: boolean;
  inUsed: PageInfo[];
}

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
  styles: any = null;
  isLoading = false;
  results: any = null;
  showDialog = false;
  selectedStyle: any = null;
  styleData: any = null;
  url: string = '';
  urlList: string[] = [];

  constructor(private fb: FormBuilder, private http: HttpClient) {
    this.form = this.fb.group({
      url: ['', [Validators.required, Validators.pattern('https?://.+')]],
    });
  }
  onFileChange(event: any) {
    const file: File = event.target.files[0];
    this.fetchUrls();

    if (file && file.type === 'application/json') {
      this.selectedFile = file;

      const reader = new FileReader();
      reader.onload = () => {
        try {
          const jsonText = reader.result as string;
          this.styleData = JSON.parse(jsonText);
        } catch (error) {
          alert('File json is not valid.');
          this.styleData = null;
        }
      };
      reader.readAsText(file);
    } else {
      this.selectedFile = null;
      this.styleData = null;
      alert('Please select a valid JSON file.');
    }
  }

  fetchUrls() {
    if (!this.url) return;

    this.http.get<string[]>(this.url).subscribe({
      next: (data) => {
        this.urlList = data;
      },
      error: (err) => {
        alert('Error fetching URLs from the provided link.');
      },
    });
  }

  onSubmit() {
    this.isLoading = true;
    if (this.form.valid && this.selectedFile) {
      const url = this.form.value.url;
      const file = this.selectedFile;

      const formData = new FormData();
      formData.append('Link', url);
      formData.append('ThemeFile', file);
      this.http.get<string[]>(url).subscribe({
        next: (data) => {
          this.urlList = data;
        },
        error: (err) => {
          alert('Error fetching URLs from the provided link.');
        },
      });

      this.http.post<any>('/api/style-detection', formData).subscribe(
        (response) => {
          this.fetchUrls();
          this.results = response.result;
          this.isLoading = false;
          this.resetFormAndFileInput();
        },
        (error) => {
          alert('Error happened: ' + error.message);
          console.error('Error:', error);
          this.isLoading = false;
        }
      );
    } else {
      alert('Please enter a valid URL and select a JSON file.');
      this.isLoading = false;
    }
  }
  resetFormAndFileInput(): void {
    this.form.reset();
    this.selectedFile = null;
    const fileInput = document.querySelector(
      'input[type="file"]'
    ) as HTMLInputElement | null;
    if (fileInput !== null) {
      fileInput.value = '';
    }
  }
  onItemClick(item: any) {
    if (item.isUsed) {
      this.openDialog(item);
    } else {
      alert(`Item: ${item.name} was not used in any categories.`);
    }
  }

  openDialog(item: Style) {
    this.selectedStyle = item;
    this.showDialog = true;
  }

  closeDialog() {
    this.selectedStyle = null;
    this.showDialog = false;
  }

  getItemsByType(type: string): Style[] {
    if (!this.results) return [];
    var s = this.results.filter((item: any) =>
      item.name.toLowerCase().startsWith(type.toLowerCase())
    );
    return s;
  }

  GetColor(): Style[] {
    if (!this.results) return [];
    var s = this.results.filter(
      (item: any) =>
        !item.name.toLowerCase().startsWith('button') &&
        !item.name.toLowerCase().startsWith('heading') &&
        !item.name.toLowerCase().startsWith('paragraph') &&
        !item.name.toLowerCase().startsWith('tcma-sb')
    );
    return s;
  }
}
