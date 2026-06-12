import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { TaskService } from './task.service';
import { LucideAngularModule, Pencil, Check, X } from 'lucide-angular';
import { supabase } from './supabase';
import { NgZone } from '@angular/core';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {

  tasks: any[] = [];
  isExpanded = true;
  inputText = '';
  editingId: number | null = null;
  editTaskText: string = '';
  readonly icons = { Pencil, Check, X };
  user: any = null;
  isLoading = false;

  constructor(private service: TaskService, private cdr: ChangeDetectorRef, private zone: NgZone) { }

  // ngOnInit(): void {
  //   console.log('ngOnInit called');
  //   this.isLoading = true;

  //   // ✅ Initial session (NO await)
  //   supabase.auth.getSession().then(({ data }) => {
  //     this.zone.run(() => {
  //       const session = data.session;

  //       if (session?.user) {
  //         this.user = session.user;
  //         this.loadTasks(this.user.id);
  //       } else {
  //         this.user = null;
  //         this.tasks = [];
  //       }

  //       this.isLoading = false;
  //     });
  //   });

  //   // ✅ Listen for login/logout
  //   supabase.auth.onAuthStateChange((event, session) => {
  //     console.log('Auth event:', event);
  //     this.zone.run(() => {
  //       console.log("Auth event:", event);

  //       if (session?.user) {
  //         this.user = session.user;
  //         this.loadTasks(this.user.id);
  //       } else {
  //         this.user = null;
  //         this.tasks = [];
  //       }
  //     });
  //   });
  // }

//   ngOnInit(): void {
//   console.log('ngOnInit called');

//   this.isLoading = true;

//   supabase.auth.onAuthStateChange((event, session) => {
//     console.log('Auth event:', event);

//     this.zone.run(() => {
//       if (session?.user) {
//         this.user = session.user;
//         this.loadTasks(this.user.id);
//       } else {
//         this.user = null;
//         this.tasks = [];
//       }

//       this.isLoading = false;
//     });
//   });
// }

ngOnInit(): void {
console.log("VERSION 2026-06-10-TEST");
  console.log('ngOnInit called');

  this.isLoading = true;

  supabase.auth.onAuthStateChange((event, session) => {

    console.log('Auth event:', event);

    this.zone.run(() => {

      this.user = session?.user ?? null;

      if (this.user) {
        this.loadTasks(this.user.id);
      } else {
        this.tasks = [];
      }

      this.isLoading = false;

      this.cdr.detectChanges();
    });

  });

}
  loadTasks(userID: string) {
    this.service.fetchTasks(userID)
      .subscribe((res: any) => {
        this.tasks = res;
        this.cdr.detectChanges();
      });;
  }

  process() {
    this.service.processTasks(this.inputText, this.user.id)
      .subscribe((res: any) => {
        this.loadTasks(this.user.id);
      });
  }

  onDelete(id: number) {
    this.service.delete(id)
      .subscribe(() => {
        this.loadTasks(this.user.id);
      });
  }

  startEdit(task: any) {
    this.editingId = task.id;
    this.editTaskText = task.task;
    task.editing = true;
  }

  saveEdit(task: any) {
    const updated = {
      ...task,
      task: this.editTaskText
    };

    this.service.update(task.id, updated)
      .subscribe(() => {
        this.editingId = null;
        this.loadTasks(this.user.id); // refresh
      });
  }

  cancelEdit(task: any) {
    this.editingId = null;
    task.editing = false;
  }

  login() {
    sessionStorage.setItem('loggedIn', 'true');
    supabase.auth.signInWithOAuth({
      provider: 'google',
      options: {
        queryParams: {
          prompt: 'select_account'
        }
      }
    });
  }

  
  async logout() {
    console.log('Logout clicked');
    this.isLoading = true;

    await supabase.auth.signOut();
  }
}