//Angular components
import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { GoogleMapsModule } from '@angular/google-maps';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MaterialModule } from './modules/material-module';

//Pages
import { AppComponent } from './app.component';
import { SideNavComponent } from './core/side-nav/side-nav.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';

import { HeaderComponent } from './core/header/header.component';
import { MapComponent } from './components/map/map.component';
import { SignalInfoComponent } from './pages/signal-info/signal-info.component';


@NgModule({
  declarations: [
    AppComponent,
    SideNavComponent,
    DashboardComponent,
    HeaderComponent,
    MapComponent,
    SignalInfoComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
    { path: '', component: DashboardComponent, pathMatch: 'full' },
    { path: 'signal-info', component: SignalInfoComponent}
], { relativeLinkResolution: 'legacy' }),
    BrowserAnimationsModule,
    MaterialModule,
    GoogleMapsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
//export class MaterialModule {}
export class AppModule { }
