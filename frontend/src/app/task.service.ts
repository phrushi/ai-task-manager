
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class TaskService {

  constructor(private http: HttpClient) { }

   public apiUrl: string = 'https://ai-task-manager-5hpa.onrender.com/api';
  //public apiUrl: string = 'http://localhost:5259/api';

  fetchTasks(userID: string) {
      const url = `${this.apiUrl}/tasks/${userID}`;

    console.log("API URL:", this.apiUrl);
    console.log("FETCH URL:", url);
    return this.http.get(url);
  }

  processTasks(input: string, userID: string) {
    return this.http.post(
      `${this.apiUrl}/tasks`,
      { input: input, userID: userID }
    );
  }

  delete(id: number) {
    return this.http.delete(`${this.apiUrl}/tasks/${id}`, {});
  }

  update(id: number, task: any) {
    return this.http.put(`${this.apiUrl}/tasks/update/${id}`, task);
  }
}
