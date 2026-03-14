type Severity = 'Low' | 'Medium' | 'High' | 'Critical' | string;

const colors: Record<string, string> = {
  Low: 'bg-green-100 text-green-700',
  Medium: 'bg-yellow-100 text-yellow-700',
  High: 'bg-orange-100 text-orange-700',
  Critical: 'bg-red-100 text-red-700',
};

export function SeverityBadge({ severity }: { severity: Severity }) {
  return (
    <span className={`inline-block text-xs font-semibold px-2 py-0.5 rounded-full ${colors[severity] ?? 'bg-gray-100 text-gray-600'}`}>
      {severity}
    </span>
  );
}
