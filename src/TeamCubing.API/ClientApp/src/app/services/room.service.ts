import { EventEmitter, Injectable } from '@angular/core';
import { ConfigurationService } from '../shared/services/configuration.service';
import { Observable, Subject } from 'rxjs';
import { Solve, SolveResult } from '../models/solve';
import { Room } from '../models/room';
import { HttpClient } from '@angular/common/http';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { AuthenticationService } from '../modules/authentication/services/authentication.service';
import { ModelResponse } from '../models/modelResponse';
import { RoomCreateRequest } from '../models/roomCreateRequest';
import { RoomDisplayDataResponse } from '../models/roomDisplayDataResponse';
import { RoomLoginRequest } from '../models/roomLoginRequest';
import { NewUserResult } from '../models/newUserResult';
import { MatDialog } from '@angular/material/dialog';
import {
  ConnectionErrorDialogComponent
} from '../components/connection-error-dialog/connection-error-dialog.component';
import { ChangePuzzleRequest } from '../models/ChangePuzzleRequest';
import { RoomPuzzle } from '../models/roomSettings';

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
  private readonly changePuzzleMethodName: string = 'ChangePuzzle';
  private readonly roomRemovedMethodName: string = 'Removed';
  private hubConnection!: HubConnection;
  private results$: Subject<SolveResult> = new Subject<SolveResult>();
  private users$: Subject<string> = new Subject<string>();
  private leftUsers$: Subject<string> = new Subject<string>();
  private solveFinished$: EventEmitter<Solve> = new EventEmitter<Solve>();
  private puzzleChanged$: Subject<RoomPuzzle> = new Subject<RoomPuzzle>();
  private roomRemoved$: Subject<void> = new Subject();

  public constructor(
    private config: ConfigurationService,
    private httpClient: HttpClient,
    private auth: AuthenticationService,
    private dialog: MatDialog,
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

  public puzzleChanged(): Observable<RoomPuzzle> {
    return this.puzzleChanged$.asObservable();
  }

  public roomRemoved(): Observable<void> {
    return this.roomRemoved$.asObservable();
  }

  public startConnection(): Promise<void> {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.config.getApiUrl() + this.hubEndpoint, {
        accessTokenFactory: () => this.auth.getToken(),
      })
      .build();
    return this.hubConnection.start().catch((err) => {
      console.log('Hub connection error: ' + err);
      this.dialog.open(ConnectionErrorDialogComponent, {
        width: '300px',
      });
    });
  }

  public subscribeOnAllRoomEvents(): void {
    this.addNewUserListener();
    this.addUsersResultsListener();
    this.addUserLeftListener();
    this.addSolveFinishedListener();
    this.addChangePuzzleListener();
    this.addRoomRemovedListener();
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

  public addChangePuzzleListener(): void {
    this.hubConnection.on(this.changePuzzleMethodName, (result: RoomPuzzle) => {
      this.puzzleChanged$.next(result);
    });
  }

  public addRoomRemovedListener(): void {
    this.hubConnection.on(this.roomRemovedMethodName, () => {
      this.roomRemoved$.next();
    });
  }

  public sendResult(request: NewUserResult): void {
    this.hubConnection.invoke(this.resultsMethodName, request).catch((err) => console.error(err));
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

  public changePuzzle(request: ChangePuzzleRequest): void {
    this.hubConnection
      .invoke(this.changePuzzleMethodName, request)
      .catch((err) => console.error(err));
  }

  public getAllRooms(): Observable<RoomDisplayDataResponse[]> {
    return this.httpClient.get<RoomDisplayDataResponse[]>(
      this.config.getApiUrl() + this.roomsEndpoint,
    );
  }

  public createRoom(request: RoomCreateRequest): Observable<ModelResponse<Room>> {
    return this.httpClient.post<ModelResponse<Room>>(
      this.config.getApiUrl() + this.roomsEndpoint,
      request,
    );
  }

  public checkAccessToRoom(roomName: string): Observable<boolean> {
    return this.httpClient.get<boolean>(
      this.config.getApiUrl() + this.roomsEndpoint + '/checkAccess/' + roomName,
    );
  }

  public loginToRoom(request: RoomLoginRequest): Observable<ModelResponse<Room>> {
    return this.httpClient.post<ModelResponse<Room>>(
      this.config.getApiUrl() + this.roomsEndpoint + '/login',
      request,
    );
  }

  public logoutRoom(): Observable<boolean> {
    return this.httpClient.get<boolean>(
      this.config.getApiUrl() + this.roomsEndpoint + '/leaveCurrentRoom/',
    );
  }

  public removeRoom(roomName: string): Observable<boolean> {
    return this.httpClient.delete<boolean>(
      this.config.getApiUrl() + this.roomsEndpoint + '/' + roomName,
    );
  }
}
