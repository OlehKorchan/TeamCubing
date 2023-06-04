import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { Solve } from '../models/solve';
import { Session } from '../models/session';
import { ConfigurationService } from '../shared/services/configuration.service';
import { AuthenticationService } from '../modules/authentication/services/authentication.service';

@Injectable({
  providedIn: 'root',
})
export class SolveService {
  private readonly sessionsStorageKey = 'sessions';

  private readonly endpoint = '/sessions';

  private userSessions: Session[] = [];

  private userSessions$: BehaviorSubject<Session[]>;

  private currentSession!: Session;

  private currentUserSession$: BehaviorSubject<Session>;

  public constructor(
    private httpClient: HttpClient,
    private config: ConfigurationService,
    private auth: AuthenticationService,
  ) {
    this.userSessions$ = new BehaviorSubject<Session[]>(this.userSessions);
    this.currentUserSession$ = new BehaviorSubject<Session>(this.currentSession);
  }

  public get sessions$(): Observable<Session[]> {
    return this.userSessions$.asObservable();
  }

  public get currentSession$(): Observable<Session> {
    return this.currentUserSession$.asObservable();
  }

  public saveUserSolve(newSolve: Solve): Observable<Solve> {
    if (this.auth.isLoggedIn()) {
      return this.saveSolveOnServer(newSolve);
    }
    return this.saveSolveLocal(newSolve);
  }

  public fetchUserSessions(): void {
    if (this.auth.isLoggedIn()) {
      this.getAllUserSessionsFromServer().subscribe({
        next: (sessions: Session[]) => {
          this.userSessions = sessions;
          this.userSessions$.next(this.userSessions);

          const currentSessionId = this.currentSession?.id ?? 0;

          if (
            this.currentSession?.id &&
            this.userSessions.find((s) => s.id === this.currentSession.id)
          ) {
            this.currentSession;
          }
        },
      });
    } else {
      this.userSessions = this.getAllUserLocalSessions();
      this.userSessions$.next(this.userSessions);
    }
  }

  public createSession(sessionName: string): Observable<Session> {
    if (this.auth.isLoggedIn()) {
      return this.createServerSession(sessionName);
    } else {
      return of(this.createLocalSession(sessionName));
    }
  }

  public createServerSession(name: string): Observable<Session> {
    return this.httpClient.post<Session>(this.getRequestUrl(), { name });
  }

  public createLocalSession(sessionName: string): Session {
    const allSessions = this.getAllUserLocalSessions();
    const newSession = {
      id: allSessions.length + 1,
      name: sessionName,
      solves: [],
    };

    allSessions.push(newSession);

    this.saveNewSessionsArrayToLocalStorage(allSessions);

    return newSession;
  }

  public removeSession(id: number): Observable<void> {
    if (this.auth.isLoggedIn()) {
      return this.removeServerSession(id);
    } else {
      return of(this.removeLocalSession(id));
    }
  }

  public removeSolve(id: number): Observable<boolean> {
    if (this.auth.isLoggedIn()) {
      return this.removeServerSolve(id);
    }
    return of(this.removeLocalSolve(id));
  }

  public clearSession(sessionId: number): void {
    if (this.auth.isLoggedIn()) {
      this.clearServerSession(sessionId);
    } else {
      this.clearLocalSession(sessionId);
    }
  }

  public getAllUserSessionsFromServer(): Observable<Session[]> {
    return this.httpClient.get<Session[]>(this.getRequestUrl());
  }

  public getUserSessionFromServer(sessionName: string): Observable<Session> {
    return this.httpClient.get<Session>(this.getRequestUrl(sessionName));
  }

  private removeServerSession(id: number): Observable<void> {
    return this.httpClient.delete<void>(this.getRequestUrl(`/${id}`));
  }

  private removeLocalSession(id: number): void {
    let sessions = this.getAllUserLocalSessions();
    sessions = sessions.filter((s) => s.id !== id);

    this.saveNewSessionsArrayToLocalStorage(sessions);
  }

  private saveSolveOnServer(newSolve: Solve): Observable<Solve> {
    return this.httpClient.post<Solve>(this.getRequestUrl('/solve'), newSolve);
  }

  private saveSolveLocal(newSolve: Solve): Observable<Solve> {
    let sessions = this.getAllUserLocalSessions();

    sessions = sessions.map<Session>((s) => {
      if (s.id === newSolve.sessionId) {
        newSolve.id = s.solves.length + 1;
        s.solves.unshift(newSolve);
      }

      return s;
    });

    this.saveNewSessionsArrayToLocalStorage(sessions);

    return of(newSolve);
  }

  private removeServerSolve(id: number): Observable<boolean> {
    return this.httpClient.delete<boolean>(this.getRequestUrl(`/solve/${id}`));
  }

  private removeLocalSolve(id: number): boolean {
    let deleted = false;
    let sessions = this.getAllUserLocalSessions();

    if (sessions) {
      sessions = sessions.map<Session>((s) => {
        s.solves = s.solves.filter((solve) => {
          if (solve.id === id) {
            deleted = true;
            return;
          }

          return solve;
        });

        return s;
      });

      if (deleted) {
        this.saveNewSessionsArrayToLocalStorage(sessions);
      }
    }

    return deleted;
  }

  private clearServerSession(sessionId: number): void {
    this.httpClient.delete(this.getRequestUrl(`/clear/${sessionId}`)).subscribe({
      next: () => console.log('Server session successfully cleared'),
      error: (error) => console.error(error),
    });
  }

  private clearLocalSession(sessionId: number): void {
    let sessions: Session[] = this.getAllUserLocalSessions();

    if (sessions.length) {
      sessions = sessions.map<Session>((s) => {
        if (s.id === sessionId) {
          s.solves = [];
        }

        return s;
      });

      this.saveNewSessionsArrayToLocalStorage(sessions);
    }
  }

  private getAllUserLocalSessions(): Session[] {
    return (
      (
        JSON.parse(localStorage.getItem(this.sessionsStorageKey) as string) as Session[]
      ) ?? [
        { id: 1, name: 'session1', solves: [] },
      ]
    );
  }

  private saveNewSessionsArrayToLocalStorage(sessions: Session[]): void {
    localStorage.setItem(this.sessionsStorageKey, JSON.stringify(sessions));
  }

  private getRequestUrl(query?: string): string {
    query = query || '';
    return this.config.getApiUrl() + this.endpoint + query;
  }
}
