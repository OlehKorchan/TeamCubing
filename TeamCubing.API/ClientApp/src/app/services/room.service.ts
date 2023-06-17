import { EventEmitter, Injectable } from '@angular/core';
import { ConfigurationService } from '../shared/services/configuration.service';
import { Observable, Subject } from 'rxjs';
import { Solve, SolveResult } from '../models/solve';
import { Room } from '../models/room';
import { HttpClient } from '@angular/common/http';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { AuthenticationService } from '../modules/authentication/services/authentication.service';
import { ModelResponse } from '../models/modelResponse';

@Injectable({
  providedIn: 'root',
})
export class RoomService {
  private readonly roomsEndpoint: string = '/rooms';
  private readonly hubEndpoint: string = '/hubs/room';
  private readonly solveFinishedMethodName: string = 'SolveFinished';
  private readonly userLeftMethodName: string = 'UserLeft';
  private readonly resultsMethodName: string = 'NewResult';
  private readonly newUsersMethodName: string = 'NewUser';
  private readonly askForNewSolveMethodName: string = 'AskForNewSolve';
  private readonly forceNewSolveMethodName: string = 'ForceNewSolve';
  private readonly joinRoomMethodName: string = 'JoinGroup';
  private readonly leaveRoomMethodName: string = 'LeaveGroup';
  private hubConnection!: HubConnection;
  private results$: Subject<SolveResult> = new Subject<SolveResult>();
  private users$: Subject<string> = new Subject<string>();
  private leftUsers$: Subject<string> = new Subject<string>();
  private solveFinished$: EventEmitter<Solve> = new EventEmitter<Solve>();

  public constructor(
    private config: ConfigurationService,
    private httpClient: HttpClient,
    private auth: AuthenticationService,
  ) {}

  public results(): Observable<SolveResult> {
    return this.results$.asObservable();
  }

  public users(): Observable<string> {
    return this.users$.asObservable();
  }

  public leftUsers(): Observable<string> {
    return this.leftUsers$.asObservable();
  }

  public solveFinished(): Observable<Solve> {
    return this.solveFinished$.asObservable();
  }

  public async startConnection(): Promise<any> {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.config.getApiUrl() + this.hubEndpoint, {
        accessTokenFactory: () => this.auth.getToken(),
      })
      .build();
    return this.hubConnection
      .start()
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
    this.hubConnection.on(this.resultsMethodName, (data: SolveResult) => {
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
    this.hubConnection.on(this.solveFinishedMethodName, (result: Solve) => {
      this.solveFinished$.emit(result);
    });
  }

  public sendResult(roomId: string, solveNumber: number, timeMilliseconds: number): void {
    this.hubConnection
      .invoke(this.resultsMethodName, roomId, solveNumber, timeMilliseconds)
      .catch((err) => console.error(err));
  }

  public joinRoom(roomName: string): void {
    this.hubConnection.invoke(this.joinRoomMethodName, roomName).catch((err) => console.error(err));
  }

  public leaveRoom(roomName: string): void {
    if (this.hubConnection) {
      this.hubConnection
        .invoke(this.leaveRoomMethodName, roomName)
        .catch((err) => console.error(err));
    }
  }

  public forceNewSolve(roomId: string): void {
    this.hubConnection
      .invoke(this.forceNewSolveMethodName, roomId)
      .catch((err) => console.error(err));
  }

  public askForNewSolve(roomId: string): void {
    this.hubConnection
      .invoke(this.askForNewSolveMethodName, roomId)
      .catch((err) => console.error(err));
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

  public loginToRoom(roomName: string, roomPassword: string): Observable<ModelResponse<Room>> {
    return this.httpClient.post<ModelResponse<Room>>(
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
