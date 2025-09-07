import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class LyricsService {
  private showLyricsSubject = new BehaviorSubject<boolean>(false);
  public showLyrics$ = this.showLyricsSubject.asObservable();

  toggleLyrics(): void {
    this.showLyricsSubject.next(!this.showLyricsSubject.value);
  }

  setShowLyrics(show: boolean): void {
    this.showLyricsSubject.next(show);
  }

  getShowLyrics(): boolean {
    return this.showLyricsSubject.value;
  }
}

