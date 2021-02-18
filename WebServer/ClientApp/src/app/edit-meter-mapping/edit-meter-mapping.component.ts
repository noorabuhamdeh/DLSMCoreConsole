import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import {Location} from '@angular/common';


@Component({
  selector: 'app-edit-meter-mapping',
  templateUrl: './edit-meter-mapping.component.html'
})
export class EditMeterMappingComponent implements OnInit {
  headers = new HttpHeaders('content-type:application/json');
  metermapping:MeterMapping = new MeterMapping();

  constructor(private router: Router,
    private location: Location,
    private _activatedRoute: ActivatedRoute,
    private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
      
    }
  ngOnInit(): void {
    let meterid = this._activatedRoute.snapshot.params['id'];
    let mappingid = this._activatedRoute.snapshot.params['mapping'];
      this.http.get<MeterMapping>(this.baseUrl + 'metermapping/' + meterid + "/" + mappingid).subscribe(result => {
        this.metermapping = result;
      });
  }
  submit(){
    this.http.put(this.baseUrl + 'metermapping', JSON.stringify(this.metermapping), {headers: this.headers})
    .subscribe((s)=>{
      this.location.back();
    });
  }
  cancel(){
    this.location.back();
  }
 }

class MeterMapping{
  id: number;
  meterId:number;
  obiS_Code:string;
  mappedToAddress:number;
  dataType:string;
}
