import { Injectable, Injector } from "@angular/core";
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable, throwError } from "rxjs";
import { catchError, tap } from 'rxjs/operators';
import { Router } from "@angular/router";
import { FilterService } from "./filter.service";
// import { isArray } from "util";

@Injectable()
export class GlobalHttpInterceptorService implements HttpInterceptor {
  filterService: FilterService;
  constructor(public router: Router, public inj: Injector) {
  }
  private responseBody: any[]
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

    return next.handle(req).pipe(
      tap(httpEvent => {
        if (httpEvent.type === 0) {
          return;
        }
        this.responseBody = httpEvent["body"];
        if (this.responseBody.length === 0) {
          console.log("empty response body");
          //load filter error msg?
          this.inj.get(FilterService).updateFilterErrorState(2);
        }
      }),
      catchError((error) => {
        console.log('API error occurred')
        console.error(error);
        //load filter error msg?
        this.inj.get(FilterService).updateFilterErrorState(2);
        return throwError(error.message);
      })
    )
  }
}
