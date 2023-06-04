import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { Session } from '../models/session';
import { SolveService } from '../services/solve.service';
import { DialogService } from '../services/dialog.service';

@Component({
  selector: 'app-sessions',
  templateUrl: './sessions.component.html',
  styleUrls: ['./sessions.component.css'],
})
export class SessionsComponent implements OnInit {
  @Output()
  public changeSession: EventEmitter<Session> = new EventEmitter<Session>();

  @Output()
  public clearSession: EventEmitter<void> = new EventEmitter<void>();

  public sessions: Session[] = [];

  public currentSession!: Session;

  public isSessionCreateMode: boolean = false;

  public newSessionName!: string;

  public constructor(private solveService: SolveService, private dialogService: DialogService) {}

  public ngOnInit() {
    // this.solveService.fetchUserSessions().subscribe({
    //   next: (sessions: Session[]) => {
    //     this.sessions = sessions;
    //     this.changeCurrentSession(this.sessions[0].id);
    //   },
    // });
  }

  public changeCurrentSession(sessionId: number): void {
    const newSession = this.sessions.find((s) => s.id === sessionId) as Session;

    if (newSession) {
      this.currentSession = newSession;
      this.changeSession.emit(this.currentSession);
    }
  }

  public onClearSession(): void {
    this.solveService.clearSession(this.currentSession.id);
    this.currentSession.solves = [];
    this.clearSession.emit();
  }

  public onRemoveSession(id: number, name: string): void {
    this.dialogService
      .showMessageDialog('DELETE SESSION', `Are you sure to delete "${name}" session`)
      .afterClosed()
      .subscribe({
        next: (res: boolean) => {
          if (res) {
            this.solveService.removeSession(id).subscribe({
              next: () => {
                this.sessions = this.sessions.filter((s: Session) => s.id !== id);
                if (id === this.currentSession.id) {
                  this.changeCurrentSession(this.sessions[0].id);
                }
              },
            });
          }
        },
      });
  }

  public toggleSessionCreateMode(): void {
    this.isSessionCreateMode = !this.isSessionCreateMode;
  }

  public onNewSession(): void {
    this.solveService.createSession(this.newSessionName).subscribe({
      next: (response: Session) => {
        this.sessions.push(response);
        this.toggleSessionCreateMode();
      },
    });
  }
}
