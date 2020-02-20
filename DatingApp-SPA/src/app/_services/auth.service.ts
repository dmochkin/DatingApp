import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  baseUrl = 'http://localhost:5000/api/auth/';

constructor(private http: HttpClient) { }

login(model: any) {
  const requestTarget = this.baseUrl + 'login';
  const request = this.http.post(requestTarget, model);
  return request.pipe(map((response: any) => {
    const user = response;
    if (user) {
      localStorage.setItem('token', user.token);
    }
  })
  );
}

register(model: any) {
  const requestTarget = this.baseUrl + 'register';
  const request = this.http.post(requestTarget, model);
  return request;
}

}
