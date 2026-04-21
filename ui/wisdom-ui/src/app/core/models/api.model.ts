export type MediaTypeEnum = 'Video' | 'Document' | 'Image';

export interface CourseApiResponse {
  courseId: string;
  teacherId: string;
  title: string;
  description: string;
  price: number;
  isPublished: boolean;
  createdAt?: string;
}

export interface CreateCourseApiRequest {
  title: string;
  description: string;
  price: number;
}

export interface CreateLessonApiRequest {
  title: string;
  description: string;
  position: number;
  isPreview: boolean;
}

export interface LessonApiResponse {
  lessonId: string;
  courseId: string;
  title: string;
  description: string;
  position: number;
  isPreview: boolean;
  createdAt?: string;
  mediaId?: string;
}

export interface MediaApiResponse {
  mediaId: string;
  lessonId: string;
  type: string;
  status: string;
  filePath: string;
  thumbnailPath?: string;
  duration?: number;
  createdAt?: string;
  fileUrl?: string;
  thumbnailUrl?: string;
}
