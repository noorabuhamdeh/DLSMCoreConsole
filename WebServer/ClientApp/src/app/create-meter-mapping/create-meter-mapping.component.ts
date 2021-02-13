import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import {Location} from '@angular/common';


@Component({
  selector: 'app-create-meter-mapping',
  templateUrl: './create-meter-mapping.component.html'
})
export class CreateMeterMappingComponent implements OnInit {
  headers = new HttpHeaders('content-type:application/json');
  meter: Meter;
  metermapping:MeterMapping = new MeterMapping();

  constructor(private router: Router,
    private location: Location,
    private _activatedRoute: ActivatedRoute,
    private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
      
    }
  ngOnInit(): void {
    let meterid = this._activatedRoute.snapshot.params['id'];
      this.http.get<Meter>(this.baseUrl + 'meter/' + meterid).subscribe(result => {
        this.meter = result;
        this.metermapping.meterId = this.meter.id;
      });
  }
  submit(){
    this.http.post(this.baseUrl + 'metermapping', JSON.stringify(this.metermapping), {headers: this.headers})
    .subscribe((s)=>{
      this.location.back();
    });
  }
  cancel(){
    this.location.back();
  }
 }

class Meter {
  id: number;
  password: string;
  physicalServer: string;
  clientAddress: string;
  manufactureName: string;
}
class MeterMapping{
  id: number;
  meterId:number;
  obis_Code:string;
  mappedToAddress:number;
  dataType:string;
}
