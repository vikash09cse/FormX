export interface LocationItem {
  id: string;
  name: string;
  code: string;
  status: string;
  statusCode: number;
  createdAt: string;
  updatedAt: string;
  parentId?: string | null;
  parentName?: string | null;
  stateId?: string | null;
  stateName?: string | null;
  districtId?: string | null;
  districtName?: string | null;
}

export interface LocationImportResult {
  imported: number;
  errorCount: number;
  errors: { rowNumber: number; message: string }[];
}
