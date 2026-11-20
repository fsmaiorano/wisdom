export type LessonContentType = 'video' | 'document';

export const LESSON_CONTENT_TYPE_LABELS: Record<LessonContentType, string> = {
  video: 'Video',
  document: 'Documents',
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
  /** Mocked filename/url for video or document content */
  contentFileName?: string;
  /** Media record ID after upload */
  mediaId?: string;
  isPreview?: boolean;
  resources: LessonResource[];
}

export interface CreateLessonRequest {
  courseId: string;
  title: string;
  description: string;
  contentType: LessonContentType;
  contentFile?: File;
  resources: File[];
}
