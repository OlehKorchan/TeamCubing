import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'inputTime',
})
export class InputTimePipe implements PipeTransform {
  public transform(value: number): string {
    if (+value) {
      const microseconds = value % 100;
      let seconds = parseInt(((value % 10000) / 100).toString());
      const minutesOverflow = parseInt((seconds / 60).toString());
      seconds -= minutesOverflow * 60;
      let minutes = parseInt(((value % 1000000) / 10000).toString()) + minutesOverflow;
      const hoursOverflow = parseInt((minutes / 60).toString());
      minutes -= hoursOverflow * 60;
      const hours = parseInt(((value % 100000000) / 1000000).toString()) + hoursOverflow;

      if (hours > 24) {
        return '';
      }

      let result: string = '';

      if (hours > 0) {
        result += hours + ':';
      }
      if (minutes > 0) {
        result += (result !== '' && minutes <= 9 ? '0' + minutes : minutes) + ':';
      }

      result += (result !== '' && seconds <= 9 ? '0' + seconds : seconds) + '.';

      result += microseconds <= 9 ? '0' + microseconds : microseconds;

      return result;
    }

    return '';
  }

  public parse(value: string | undefined): number | '' {
    if (!value) {
      return '';
    }

    const times = value.split(':');
    if (times?.length) {
      if (times.length === 3) {
        const hours = parseInt(times[0]);

        return hours * 1000000 + this.parseMinutesPart(times) + this.parseSecondsPart(times);
      } else if (times.length === 2) {
        return this.parseMinutesPart(times) + this.parseSecondsPart(times);
      } else if (times.length === 1) {
        return this.parseSecondsPart(times);
      }
    }

    return '';
  }

  private parseMinutesPart(times: string[]): number {
    const minutes = parseInt(times.at(-2) as string);

    return minutes * 10000;
  }

  private parseSecondsPart(times: string[]): number {
    const secondsTime = (times.at(-1) as string).split('.');
    const microseconds = parseInt(secondsTime[1]);
    const seconds = parseInt(secondsTime[0]);

    return microseconds + seconds * 100;
  }
}
