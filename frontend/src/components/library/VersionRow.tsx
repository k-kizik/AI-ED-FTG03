import type { DocumentVersionDto } from '../../types/api';

interface Props {
  version: DocumentVersionDto;
  documentId: string;
  documentName: string;
  checked: boolean;
  onToggle: () => void;
}

function formatBytes(bytes: number) {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(0)} KB`;
  return `${(bytes / 1024 / 1024).toFixed(1)} MB`;
}

export function VersionRow({ version, checked, onToggle }: Props) {
  return (
    <tr className="hover:bg-gray-50 transition-colors">
      <td className="px-4 py-2">
        <input
          type="checkbox"
          checked={checked}
          onChange={onToggle}
          className="w-4 h-4 accent-blue-600 cursor-pointer"
        />
      </td>
      <td className="px-4 py-2 text-sm font-medium text-gray-800">{version.versionNumber}</td>
      <td className="px-4 py-2 text-sm text-gray-600 max-w-xs truncate">{version.fileName}</td>
      <td className="px-4 py-2 text-sm text-gray-500">{version.pageCount} pg</td>
      <td className="px-4 py-2 text-sm text-gray-500">{formatBytes(version.fileSizeBytes)}</td>
      <td className="px-4 py-2 text-sm text-gray-400">
        {new Date(version.uploadedAt).toLocaleDateString()}
      </td>
    </tr>
  );
}
