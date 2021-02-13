import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';

@Component({
  selector: 'app-meter-mapping',
  templateUrl: './meter-mapping.component.html'
})
export class MeterMappingComponent {
  meter: Meter;
  meterMappings:MeterMapping[];

  constructor(private location: Location,
    private _activatedRoute: ActivatedRoute,
    private router: Router, http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    let meterid = this._activatedRoute.snapshot.params['id'];
    http.get<Meter>(baseUrl + 'meter/' + meterid).subscribe(result => {
      this.meter = result;
      http.get<MeterMapping[]>(baseUrl + 'metermapping/' + this.meter.id).subscribe(mappings=>{
        this.meterMappings = mappings;
      }, error => console.error(error));
    }, error => console.error(error));

  }
  createMeterMapping() {
    this.router.navigateByUrl("create-meter-mapping/" + this.meter.id );
  }
  back(){
    this.location.back();
  }
  editMeterMapping(meterMappingId){
    this.router.navigateByUrl("meters-edit/" + this.meter.id + "/" + meterMappingId);
  }
}

interface Meter {
  id: number;
  name:string;
  objectsXMLDocument:string;
  password: string;
  physicalServer: string;
  clientAddress: string;
  manufactureName: string;
}

class MeterMapping{
  id: number;
  meterId:number;
  obiS_Code:string;
  mappedToAddress:number;
  dataType:string;
}
