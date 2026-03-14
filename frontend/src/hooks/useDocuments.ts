import { useQuery } from '@tanstack/react-query';
import { getDocuments } from '../api/documents';

export function useDocuments(includeAllUsers = false) {
  return useQuery({
    queryKey: ['documents', includeAllUsers],
    queryFn: () => getDocuments(includeAllUsers),
    select: (data) => data.documents,
  });
}
