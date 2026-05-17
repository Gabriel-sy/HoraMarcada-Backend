ALTER TABLE stores ENABLE ROW LEVEL SECURITY;
CREATE POLICY store_owner ON stores
  USING (user_id = auth.uid());

ALTER TABLE professionals ENABLE ROW LEVEL SECURITY;
CREATE POLICY prof_owner ON professionals
  USING (store_id IN (SELECT id FROM stores WHERE user_id = auth.uid()));

ALTER TABLE appointments ENABLE ROW LEVEL SECURITY;
CREATE POLICY appt_owner_read ON appointments
  FOR SELECT USING (store_id IN (SELECT id FROM stores WHERE user_id = auth.uid()));
CREATE POLICY appt_public_insert ON appointments
  FOR INSERT WITH CHECK (true);
