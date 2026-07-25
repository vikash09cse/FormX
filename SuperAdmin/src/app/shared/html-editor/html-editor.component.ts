import { Component, ElementRef, ViewChild, forwardRef, input } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-html-editor',
  standalone: true,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => HtmlEditorComponent),
      multi: true
    }
  ],
  template: `
    <div class="html-editor">
      <div class="toolbar">
        <button type="button" (click)="cmd('bold')" title="Bold"><b>B</b></button>
        <button type="button" (click)="cmd('italic')" title="Italic"><i>I</i></button>
        <button type="button" (click)="cmd('underline')" title="Underline"><u>U</u></button>
        <button type="button" (click)="cmd('insertUnorderedList')" title="Bullet list">• List</button>
        <button type="button" (click)="cmd('insertOrderedList')" title="Numbered list">1. List</button>
        <button type="button" (click)="insertLink()" title="Link">Link</button>
        <button type="button" (click)="toggleSource()" title="HTML source">{{ sourceMode ? 'Visual' : 'HTML' }}</button>
      </div>
      @if (sourceMode) {
        <textarea class="source" [value]="value" (input)="onSource($event)" [disabled]="disabled" rows="12"></textarea>
      } @else {
        <div
          #editor
          class="surface"
          contenteditable="true"
          [attr.data-placeholder]="placeholder()"
          (input)="onInput()"
          (blur)="onTouched()"
        ></div>
      }
    </div>
  `,
  styles: [`
    .html-editor { border: 1px solid #cbd5e1; border-radius: 8px; overflow: hidden; background: #fff; }
    .toolbar { display: flex; flex-wrap: wrap; gap: 0.35rem; padding: 0.45rem; background: #f8fafc; border-bottom: 1px solid #e2e8f0; }
    .toolbar button { border: 1px solid #cbd5e1; background: #fff; border-radius: 4px; padding: 0.25rem 0.5rem; cursor: pointer; font-size: 0.8rem; }
    .surface { min-height: 180px; padding: 0.75rem; outline: none; }
    .surface:empty:before { content: attr(data-placeholder); color: #94a3b8; }
    .source { width: 100%; border: 0; padding: 0.75rem; font-family: ui-monospace, monospace; font-size: 0.85rem; resize: vertical; box-sizing: border-box; }
  `]
})
export class HtmlEditorComponent implements ControlValueAccessor {
  readonly placeholder = input('Write content…');
  @ViewChild('editor') editorRef?: ElementRef<HTMLDivElement>;

  value = '';
  disabled = false;
  sourceMode = false;
  private onChange: (v: string) => void = () => undefined;
  onTouched: () => void = () => undefined;

  writeValue(value: string | null): void {
    this.value = value ?? '';
    queueMicrotask(() => this.syncDom());
  }

  registerOnChange(fn: (v: string) => void): void { this.onChange = fn; }
  registerOnTouched(fn: () => void): void { this.onTouched = fn; }
  setDisabledState(isDisabled: boolean): void { this.disabled = isDisabled; }

  cmd(command: string) {
    document.execCommand(command, false);
    this.onInput();
  }

  insertLink() {
    const url = window.prompt('Link URL');
    if (!url) return;
    document.execCommand('createLink', false, url);
    this.onInput();
  }

  toggleSource() {
    if (!this.sourceMode && this.editorRef) {
      this.value = this.editorRef.nativeElement.innerHTML;
    }
    this.sourceMode = !this.sourceMode;
    queueMicrotask(() => this.syncDom());
  }

  onInput() {
    if (!this.editorRef) return;
    this.value = this.editorRef.nativeElement.innerHTML;
    this.onChange(this.value);
  }

  onSource(event: Event) {
    this.value = (event.target as HTMLTextAreaElement).value;
    this.onChange(this.value);
  }

  private syncDom() {
    if (this.sourceMode || !this.editorRef) return;
    if (this.editorRef.nativeElement.innerHTML !== this.value) {
      this.editorRef.nativeElement.innerHTML = this.value;
    }
  }
}
