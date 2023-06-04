import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'msToTime',
})
export class MsToTimePipe implements PipeTransform {
  public transform(milliseconds: number): string {
    if (milliseconds < 0) {
      return '(' + this.transformMsToTime(-milliseconds) + ') DNF';
    }

    return this.transformMsToTime(milliseconds);
  }

  private transformMsToTime(milliseconds: number): string {
    let seconds = Math.floor(milliseconds / 1000);
    let minutes = Math.floor(seconds / 60);

    seconds %= 60;
    minutes %= 60;
    milliseconds %= 1000;
    milliseconds = Math.round(milliseconds / 10);

    const minutesPart = minutes ? `${minutes}:` : '';
    const millisecondsPart = milliseconds < 10 ? `0${milliseconds}` : milliseconds;

    return `${minutesPart}${seconds}.${millisecondsPart}`;
  }
}
