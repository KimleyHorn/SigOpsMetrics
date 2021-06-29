import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { ContactComponent } from '../core/contact-form/contact-form';

@Injectable({
  providedIn: 'root'
})
export class ContactFormService {
  baseUrl: string;

  constructor(private http: HttpClient, @Inject('BASE_URL') baseUrlInject: string) {
    this.baseUrl = baseUrlInject;
  }

  submitData(data: any): Observable<any> {
    return this.http.post(this.baseUrl + 'signals/contact-us', data);     
  }
}

