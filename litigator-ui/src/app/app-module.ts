import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { DxButtonModule } from 'devextreme-angular/ui/button';
import { DxLoadIndicatorModule } from 'devextreme-angular/ui/load-indicator';
import { DxDataGridModule } from 'devextreme-angular/ui/data-grid';
import { DxChartModule } from 'devextreme-angular/ui/chart';
import { DxPieChartModule } from 'devextreme-angular/ui/pie-chart';
import { DxTooltipModule } from 'devextreme-angular/ui/tooltip';

import { App } from './app';
import { HomeComponent } from './pages/home/home.component';

@NgModule({
  declarations: [
    App
  ],
  imports: [
    BrowserModule,
    HttpClientModule,  // Add this for your HTTP services
    FormsModule,
    ReactiveFormsModule,

    DxButtonModule,
    DxLoadIndicatorModule,
    DxDataGridModule,
    DxChartModule,
    DxPieChartModule,
    DxTooltipModule,
    HomeComponent
  ],
  providers: [],
  bootstrap: [App]
})
export class AppModule { }
