import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';


@Component({
  selector: 'app-meters',
  templateUrl: './create-meter.component.html'
})
export class CreateMeterComponent implements OnInit {
  headers = new HttpHeaders('content-type:application/json');
  meter: Meter = new Meter();
 
  constructor(private router: Router,
    private _activatedRoute: ActivatedRoute,
    private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
  }
  ngOnInit(): void {
    //this.id = this._activatedRoute.snapshot.queryParams['id'];
  }
  submit(){
    this.http.post(this.baseUrl + 'meter', JSON.stringify(this.meter), {headers: this.headers})
    .subscribe((s)=>{
      console.log("saved...");
      this.router.navigate(['/meters']);
    });
  }
  cancel(){
    this.router.navigate(['/meters']);
  }
 }

class Meter {
  id: number;
  name:string;
  objectsXMLDocument:string;
  password: string;
  physicalServer: string;
  clientAddress: string;
  manufactureName: string;
}
