interface Props {
  message?: string;
}

export function LoadingOverlay({ message = 'Loading…' }: Props) {
  return (
    <div className="fixed inset-0 z-50 flex flex-col items-center justify-center bg-black/50">
      <div className="bg-white rounded-xl shadow-xl p-8 flex flex-col items-center gap-4 max-w-sm w-full mx-4">
        <div className="w-10 h-10 border-4 border-blue-600 border-t-transparent rounded-full animate-spin" />
        <p className="text-gray-700 text-center text-sm">{message}</p>
      </div>
    </div>
  );
}
