import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
  selector: 'app-meters',
  templateUrl: './meters.component.html'
})
export class MetersComponent {
  public meters: Meter[];

  constructor(private router: Router, http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    http.get<Meter[]>(baseUrl + 'meter').subscribe(result => {
      this.meters = result;
    }, error => console.error(error));
  }
  createMeter() {
    this.router.navigate(['create-meter']);
  }
  editMeter(id){
    this.router.navigateByUrl('meters/' + id);
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
