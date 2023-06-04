import { Injectable, EventEmitter } from '@angular/core';
import { ConfigurationService } from '../shared/services/configuration.service';
import { Observable, Subject } from 'rxjs';
import { IRoomSolve, ISolveResult } from '../models/roomSolve';
import { IRoomLoginResult } from '../models/roomLoginResult';
import { HttpClient } from '@angular/common/http';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { AuthenticationService } from '../modules/authentication/services/authentication.service';

@Injectable({
  providedIn: 'root',
})
export class RoomService {
  private readonly roomsEndpoint: string = '/rooms';
  private readonly hubEndpoint: string = '/hubs/room';
  private readonly solveFinishedMethodName: string = 'SolveFinished';
  private readonly userLeftMethodName: string = 'UserLeft';
  private readonly resultsMethodName: string = 'Send';
  private readonly newUsersMethodName: string = 'NewUser';
  private hubConnection!: HubConnection;
  private results$: Subject<ISolveResult> = new Subject<ISolveResult>();
  private users$: Subject<string> = new Subject<string>();
  private leftUsers$: Subject<string> = new Subject<string>();
  private solveFinished$: EventEmitter<IRoomSolve> = new EventEmitter<IRoomSolve>();

  public constructor(
    private config: ConfigurationService,
    private httpClient: HttpClient,
    private auth: AuthenticationService,
  ) {}

  public results(): Observable<ISolveResult> {
    return this.results$.asObservable();
  }

  public users(): Observable<string> {
    return this.users$.asObservable();
  }

  public leftUsers(): Observable<string> {
    return this.leftUsers$.asObservable();
  }

  public solveFinished(): Observable<IRoomSolve> {
    return this.solveFinished$.asObservable();
  }

  public startConnection(): Promise<any> {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.config.getApiUrl() + this.hubEndpoint, {
        accessTokenFactory: () => this.auth.getToken(),
      })
      .build();
    return this.hubConnection
      .start()
      .then(() => console.log('Connection started'))
      .catch((err) => console.log('Error while starting connection: ' + err));
  }

  public subscribeOnAllRoomEvents(): void {
    this.addNewUserListener();
    this.addUsersResultsListener();
    this.addUserLeftListener();
    this.addSolveFinishedListener();
  }

  public addUserLeftListener(): void {
    this.hubConnection.on(this.userLeftMethodName, (data: string) => {
      this.leftUsers$.next(data);
      console.log(data);
    });
  }

  public addUsersResultsListener(): void {
    this.hubConnection.on(this.resultsMethodName, (data: ISolveResult) => {
      this.results$.next(data);
      console.log(data);
    });
  }

  public addNewUserListener(): void {
    this.hubConnection.on(this.newUsersMethodName, (data: string) => {
      this.users$.next(data);
      console.log(data);
    });
  }

  public addSolveFinishedListener(): void {
    this.hubConnection.on(this.solveFinishedMethodName, (result: IRoomSolve) => {
      this.solveFinished$.emit(result);
    });
  }

  public sendResult(roomId: number, solveId: number, timeMilliseconds: number): void {
    this.hubConnection
      .invoke(this.resultsMethodName, roomId, solveId, timeMilliseconds)
      .catch((err) => console.error(err));
  }

  public joinRoom(roomName: string): void {
    this.hubConnection.invoke('JoinGroup', roomName).catch((err) => console.error(err));
  }

  public leaveRoom(roomName: string): void {
    this.hubConnection.invoke('LeaveGroup', roomName).catch((err) => console.error(err));
  }

  public forceNewSolve(roomId: number): void {
    this.hubConnection.invoke('ForceNewSolve', roomId).catch((err) => console.error(err));
  }

  public newSolve(roomId: number): void {
    this.hubConnection.invoke('NewSolve', roomId).catch((err) => console.error(err));
  }

  public getAllRooms(): Observable<string[]> {
    return this.httpClient.get<string[]>(this.config.getApiUrl() + this.roomsEndpoint);
  }

  public createRoom(roomName: string, roomPassword: string): Observable<boolean> {
    return this.httpClient.post<boolean>(this.config.getApiUrl() + this.roomsEndpoint, {
      roomName: roomName,
      roomPassword: roomPassword,
    });
  }

  public checkAccessToRoom(roomName: string): Observable<boolean> {
    return this.httpClient.get<boolean>(
      this.config.getApiUrl() + this.roomsEndpoint + '/checkAccess/' + roomName,
    );
  }

  public loginToRoom(roomName: string, roomPassword: string): Observable<IRoomLoginResult> {
    return this.httpClient.post<IRoomLoginResult>(
      this.config.getApiUrl() + this.roomsEndpoint + '/login',
      {
        roomName: roomName,
        roomPassword: roomPassword,
      },
    );
  }

  public logoutRoom(): Observable<boolean> {
    return this.httpClient.get<boolean>(
      this.config.getApiUrl() + this.roomsEndpoint + '/leaveCurrentRoom/',
    );
  }
}
