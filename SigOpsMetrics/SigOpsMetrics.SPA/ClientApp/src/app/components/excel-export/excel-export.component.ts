import { Component, Input, OnInit } from '@angular/core';
import { saveAs } from 'file-saver';

@Component({
  selector: 'excel-export',
  templateUrl: './excel-export.component.html',
  styleUrls: ['./excel-export.component.css']
})
export class ExcelExportComponent implements OnInit {
   @Input() data: any;

  constructor() { }

  ngOnInit(): void {

  }

  excelExport(): void {
    const replacer = (key, value) => (value === null ? '' : value); // specify how you want to handle null values here
    const header = Object.keys(this.data[0]);
    const csv = this.data.map((row) =>
      header
        .map((fieldName) => JSON.stringify(row[fieldName], replacer))
        .join(',')
    );
    csv.unshift(header.join(','));
    const csvArray = csv.join('\r\n');
  
    const a = document.createElement('a');
    const blob = new Blob([csvArray], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
  
    a.href = url;
    a.download = 'Export.csv';
    a.click();
    window.URL.revokeObjectURL(url);
    a.remove();
  }
}
