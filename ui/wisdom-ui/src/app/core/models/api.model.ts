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

// ── BFF aggregation responses ────────────────────────────────────────────────

export interface CourseProgressSummary {
  completedLessons: number;
  totalLessons: number;
  percentage: number;
}

export interface LessonProgress {
  watchedSeconds: number;
  isCompleted: boolean;
}

export interface LessonWithProgress {
  lessonId: string;
  courseId: string;
  title: string;
  description: string;
  position: number;
  isPreview: boolean;
  createdAt?: string;
  mediaId?: string;
  progress?: LessonProgress;
}

export interface EnrollmentInfo {
  isEnrolled: boolean;
  enrolledAt?: string;
}

export interface CoursePageResponse {
  courseId: string;
  teacherId: string;
  title: string;
  description: string;
  price: number;
  isPublished: boolean;
  createdAt?: string;
  enrollment: EnrollmentInfo;
  lessons: LessonWithProgress[];
  progress: CourseProgressSummary;
}

export interface EnrolledCourseItem {
  courseId: string;
  title: string;
  description: string;
  price: number;
  enrolledAt?: string;
  progress: CourseProgressSummary;
}

export interface DashboardResponse {
  enrolledCourses: EnrolledCourseItem[];
}

// ── Order ────────────────────────────────────────────────────────────────────

export interface PurchaseCourseRequest {
  courseId: string;
  price: number;
}

export interface PurchaseCourseResponse {
  purchaseId: string;
  userId: string;
  courseId: string;
  price: number;
  createdAt?: string;
}

// ── Progress ─────────────────────────────────────────────────────────────────

export interface UpdateProgressRequest {
  lessonId: string;
  watchedSeconds: number;
  isCompleted: boolean;
}

export interface ProgressResponse {
  progressId: string;
  userId: string;
  lessonId: string;
  watchedSeconds: number;
  isCompleted: boolean;
  updatedAt?: string;
}
