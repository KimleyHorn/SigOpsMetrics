import {Component, OnInit} from '@angular/core';
import { LegacyDialogPosition as DialogPosition, MatLegacyDialog as MatDialog } from '@angular/material/legacy-dialog';
import { MatLegacySnackBar as MatSnackBar, MatLegacySnackBarConfig as MatSnackBarConfig } from '@angular/material/legacy-snack-bar';
import { ContactFormService } from 'src/app/services/contact-form.service';

  @Component({
    selector: 'contact-form',
    templateUrl: "./contact-form.html",
    styleUrls: ['./contact-form.css']
  })

  export class ContactComponent implements OnInit {
      model: any = {};
      dialogPosition: DialogPosition = {
        top: '50px',
        right: '2%'
      }
    constructor(public dialog: MatDialog, private service: ContactFormService, private snackbar: MatSnackBar) {}

    matbarConfig: MatSnackBarConfig = {
      horizontalPosition: 'end',
      verticalPosition: 'bottom',
      duration: 5000,

    }
    ngOnInit(): void {}

     processForm() {
      this.service.submitData(this.model).subscribe(data => {
        if (data == 1) {
          this.matbarConfig.panelClass = ['contact-mat-bar-success'];
          this.snackbar.open("Message saved successfully.","",this.matbarConfig)
          this.dialog.closeAll();
        } else {
          this.matbarConfig.panelClass = ['contact-mat-bar-fail'];
          this.snackbar.open("An error occurred. Please try again.","",this.matbarConfig)
        }
      })
    }

    toggle() {
      this.dialog.open(ContactComponent,{
        width: '450px',
        position: this.dialogPosition
      });
    }

  }
