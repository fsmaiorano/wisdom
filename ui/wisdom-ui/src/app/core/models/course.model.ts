export interface Course {
  id: string;
  teacherId: string;
  title: string;
  description: string;
  price: number;
  isPublished: boolean;
}

export interface CreateCourseRequest {
  teacherId: string;
  title: string;
  description: string;
  price: number;
}
