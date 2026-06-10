
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class TaskService {

  constructor(private http: HttpClient) { }

  fetchTasks(userID: string) {
    return this.http.get(`https://ai-task-manager-5hpa.onrender.com/api//tasks/${userID}`);
  }

  processTasks(input: string, userID: string) {
    return this.http.post(
      'https://ai-task-manager-5hpa.onrender.com/api/tasks',
      { input: input, userID: userID }
    );
  }

  markComplete(id: number) {
    return this.http.put(`https://ai-task-manager-5hpa.onrender.com/api/tasks/complete/${id}`, {});
  }

  update(id: number, task: any) {
    return this.http.put(`https://ai-task-manager-5hpa.onrender.com/api/tasks/update/${id}`, task);
  }
}
