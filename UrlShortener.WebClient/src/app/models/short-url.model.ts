export interface ShortUrlDto {
  id: number;
  originalUrl: string;
  shortCode: string;
  createdBy: string;
  createdDate: string;
}

export interface ShortUrlView extends ShortUrlDto {
  shortUrl: string; 
}
