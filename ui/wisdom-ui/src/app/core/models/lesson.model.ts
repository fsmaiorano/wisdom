export type LessonContentType = 'video' | 'slides' | 'text';

export const LESSON_CONTENT_TYPE_LABELS: Record<LessonContentType, string> = {
  video: 'Video',
  slides: 'Slides',
  text: 'Text',
};

export interface LessonResource {
  id: string;
  name: string;
  mimeType: string;
  size: number;
}

export interface Lesson {
  id: string;
  courseId: string;
  order: number;
  title: string;
  description: string;
  contentType: LessonContentType;
  /** Mocked filename/url for video or slides content */
  contentFileName?: string;
  /** Plain text body when contentType is 'text' */
  contentText?: string;
  resources: LessonResource[];
}

export interface CreateLessonRequest {
  courseId: string;
  title: string;
  description: string;
  contentType: LessonContentType;
  contentFile?: File;
  contentText?: string;
  resources: File[];
}
