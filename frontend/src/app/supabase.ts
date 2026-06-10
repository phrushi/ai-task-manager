import { createClient } from '@supabase/supabase-js';

export const supabase = createClient(
  'https://vwgfdypykhbeiljeqvdj.supabase.co',
  'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InZ3Z2ZkeXB5a2hiZWlsamVxdmRqIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzUwMTc1MzQsImV4cCI6MjA5MDU5MzUzNH0.IOfo9G6ROfiaH6Js4TjQ-Kdt0ti5qAEEbKaJwG9MV0c'
);