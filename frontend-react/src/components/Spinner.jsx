export default function Spinner({ label }) {
  return (
    <div className="inline-flex items-center gap-2">
      <span className="animate-spin h-4 w-4 border-t-2 border-gray-500 rounded-full"></span>
      {label && <span className="text-sm text-gray-500">{label}</span>}
    </div>
  );
}
