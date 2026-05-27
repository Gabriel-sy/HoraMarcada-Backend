DO $$
DECLARE
    v_user_id UUID;
    v_store_id UUID := 'e7a5c8b2-5f3d-4c91-bb27-0248c8b417e2';
    v_professional_1 UUID := 'f1a9a8b7-6c5d-4e3f-b2a1-098765432101';
    v_professional_2 UUID := 'f2a9a8b7-6c5d-4e3f-b2a1-098765432102';
    v_professional_3 UUID := 'f3a9a8b7-6c5d-4e3f-b2a1-098765432103';
    v_service_1 UUID := 'a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d';
    v_service_2 UUID := 'b2c3d4e5-f6a7-8b9c-0d1e-2f3a4b5c6d7e';
    v_service_3 UUID := 'c3d4e5f6-a7b8-9c0d-1e2f-3a4b5c6d7e8f';
    v_service_4 UUID := 'd4e5f6a7-b8c9-0d1e-2f3a-4b5c6d7e8f9a';
BEGIN
    SELECT id INTO v_user_id FROM auth.users ORDER BY created_at LIMIT 1;

    IF v_user_id IS NULL THEN
        v_user_id := 'd0b8f36a-2d9d-4e9f-9f7a-8f9f8f9f8f9f';
        INSERT INTO auth.users (
            id, 
            email, 
            encrypted_password, 
            email_confirmed_at, 
            role, 
            aud, 
            raw_app_meta_data, 
            raw_user_meta_data, 
            created_at, 
            updated_at
        )
        VALUES (
            v_user_id, 
            'contato@barbeariaseed.com', 
            '$2a$10$w8uD94XG.dI09f7aR3a7u.Rpe21.sN2oQ0sF8KjWv3m6U8u0gX2eO',
            NOW(), 
            'authenticated', 
            'authenticated', 
            '{"provider": "email", "providers": ["email"]}', 
            '{"name": "Dono da Barbearia"}', 
            NOW(), 
            NOW()
        ) ON CONFLICT (id) DO NOTHING;
    END IF;

    DELETE FROM appointments WHERE store_id = v_store_id;
    DELETE FROM service_professional WHERE service_id IN (v_service_1, v_service_2, v_service_3, v_service_4);
    DELETE FROM weekly_hours WHERE professional_id IN (v_professional_1, v_professional_2, v_professional_3);
    DELETE FROM professionals WHERE store_id = v_store_id;
    DELETE FROM services WHERE store_id = v_store_id;
    DELETE FROM stores WHERE id = v_store_id;

    INSERT INTO stores (id, user_id, name, slug, logo_url, address, address_lat, address_lng, whatsapp, active, created_at)
    VALUES (
        v_store_id, 
        v_user_id, 
        'Barbearia Dom Pedro', 
        'barbearia-dom-pedro', 
        'https://images.unsplash.com/photo-1503951914875-452162b0f3f1?w=200&h=200&fit=crop', 
        'Av. Paulista, 1000 - Bela Vista, São Paulo - SP', 
        -23.561684, 
        -46.655981, 
        '+5511999999999', 
        true, 
        NOW()
    );

    INSERT INTO professionals (id, store_id, name, photo_url, whatsapp, active, created_at)
    VALUES 
    (v_professional_1, v_store_id, 'Carlos Barbudo', 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150&h=150&fit=crop', '+5511988888881', true, NOW()),
    (v_professional_2, v_store_id, 'Felipe Tesoura', 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&h=150&fit=crop', '+5511988888882', true, NOW()),
    (v_professional_3, v_store_id, 'Mariana Navalha', 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150&h=150&fit=crop', '+5511988888883', true, NOW());

    FOR d IN 2..6 LOOP
        INSERT INTO weekly_hours (id, professional_id, day_of_week, start_time, end_time, lunch_start, lunch_end, interval_min)
        VALUES (gen_random_uuid(), v_professional_1, d, '09:00:00', '19:00:00', '12:00:00', '13:00:00', 30);
    END LOOP;

    FOR d IN 1..5 LOOP
        INSERT INTO weekly_hours (id, professional_id, day_of_week, start_time, end_time, lunch_start, lunch_end, interval_min)
        VALUES (gen_random_uuid(), v_professional_2, d, '10:00:00', '20:00:00', '13:00:00', '14:00:00', 45);
    END LOOP;

    FOR d IN 0..6 LOOP
        IF d IN (0, 3, 4, 5, 6) THEN
            INSERT INTO weekly_hours (id, professional_id, day_of_week, start_time, end_time, lunch_start, lunch_end, interval_min)
            VALUES (gen_random_uuid(), v_professional_3, d, '08:00:00', '18:00:00', '11:30:00', '12:30:00', 30);
        END IF;
    END LOOP;

    INSERT INTO services (id, store_id, name, duration_min, price, active)
    VALUES 
    (v_service_1, v_store_id, 'Corte de Cabelo Masculino', 30, 50.00, true),
    (v_service_2, v_store_id, 'Barba Terapia', 30, 40.00, true),
    (v_service_3, v_store_id, 'Combo: Cabelo & Barba', 60, 80.00, true),
    (v_service_4, v_store_id, 'Corte de Cabelo Infantil', 30, 45.00, true);

    INSERT INTO service_professional (service_id, professional_id) VALUES 
    (v_service_1, v_professional_1),
    (v_service_2, v_professional_1),
    (v_service_3, v_professional_1);

    INSERT INTO service_professional (service_id, professional_id) VALUES 
    (v_service_1, v_professional_2),
    (v_service_3, v_professional_2),
    (v_service_4, v_professional_2);

    INSERT INTO service_professional (service_id, professional_id) VALUES 
    (v_service_1, v_professional_3),
    (v_service_2, v_professional_3),
    (v_service_3, v_professional_3),
    (v_service_4, v_professional_3);

    INSERT INTO appointments (id, store_id, professional_id, service_id, customer_name, customer_whatsapp, customer_email, start_time, end_time, status, cancel_token, cancel_token_used, created_at)
    VALUES (
        gen_random_uuid(), 
        v_store_id, 
        v_professional_1, 
        v_service_1, 
        'João da Silva', 
        '+5511977777771', 
        'joao.silva@email.com',
        (CURRENT_DATE - INTERVAL '1 day' + TIME '10:00:00')::timestamp with time zone, 
        (CURRENT_DATE - INTERVAL '1 day' + TIME '10:30:00')::timestamp with time zone, 
        'completed', 
        'token-past-1', 
        false, 
        (CURRENT_DATE - INTERVAL '2 days')
    );

    INSERT INTO appointments (id, store_id, professional_id, service_id, customer_name, customer_whatsapp, customer_email, start_time, end_time, status, cancel_token, cancel_token_used, created_at)
    VALUES (
        gen_random_uuid(), 
        v_store_id, 
        v_professional_2, 
        v_service_3, 
        'Pedro Pedrosa', 
        '+5511977777772', 
        'pedro.pedrosa@email.com',
        (CURRENT_DATE - INTERVAL '1 day' + TIME '14:00:00')::timestamp with time zone, 
        (CURRENT_DATE - INTERVAL '1 day' + TIME '15:00:00')::timestamp with time zone, 
        'canceled', 
        'token-past-2', 
        true, 
        (CURRENT_DATE - INTERVAL '2 days')
    );

    INSERT INTO appointments (id, store_id, professional_id, service_id, customer_name, customer_whatsapp, customer_email, start_time, end_time, status, cancel_token, cancel_token_used, created_at)
    VALUES (
        gen_random_uuid(), 
        v_store_id, 
        v_professional_1, 
        v_service_1, 
        'Lucas Santos', 
        '+5511977777773', 
        'lucas.santos@email.com',
        (CURRENT_DATE + TIME '09:30:00')::timestamp with time zone, 
        (CURRENT_DATE + TIME '10:00:00')::timestamp with time zone, 
        'confirmed', 
        'token-today-1', 
        false, 
        (CURRENT_DATE - INTERVAL '1 day')
    );

    INSERT INTO appointments (id, store_id, professional_id, service_id, customer_name, customer_whatsapp, customer_email, start_time, end_time, status, cancel_token, cancel_token_used, created_at)
    VALUES (
        gen_random_uuid(), 
        v_store_id, 
        v_professional_1, 
        v_service_2, 
        'Marcos Souza', 
        '+5511977777774', 
        'marcos.souza@email.com',
        (CURRENT_DATE + TIME '10:00:00')::timestamp with time zone, 
        (CURRENT_DATE + TIME '10:30:00')::timestamp with time zone, 
        'confirmed', 
        'token-today-2', 
        false, 
        (CURRENT_DATE - INTERVAL '1 day')
    );

    INSERT INTO appointments (id, store_id, professional_id, service_id, customer_name, customer_whatsapp, customer_email, start_time, end_time, status, cancel_token, cancel_token_used, created_at)
    VALUES (
        gen_random_uuid(), 
        v_store_id, 
        v_professional_3, 
        v_service_3, 
        'Roberto Oliveira', 
        '+5511977777775', 
        'roberto.oliveira@email.com',
        (CURRENT_DATE + TIME '15:00:00')::timestamp with time zone, 
        (CURRENT_DATE + TIME '16:00:00')::timestamp with time zone, 
        'confirmed', 
        'token-today-3', 
        false, 
        (CURRENT_DATE - INTERVAL '1 day')
    );

    INSERT INTO appointments (id, store_id, professional_id, service_id, customer_name, customer_whatsapp, customer_email, start_time, end_time, status, cancel_token, cancel_token_used, created_at)
    VALUES (
        gen_random_uuid(), 
        v_store_id, 
        v_professional_2, 
        v_service_4, 
        'Gustavo (Pai: Thiago)', 
        '+5511977777776', 
        'thiago.pai@email.com',
        (CURRENT_DATE + INTERVAL '1 day' + TIME '11:00:00')::timestamp with time zone, 
        (CURRENT_DATE + INTERVAL '1 day' + TIME '11:30:00')::timestamp with time zone, 
        'confirmed', 
        'token-tomorrow-1', 
        false, 
        NOW()
    );

    INSERT INTO appointments (id, store_id, professional_id, service_id, customer_name, customer_whatsapp, customer_email, start_time, end_time, status, cancel_token, cancel_token_used, created_at)
    VALUES (
        gen_random_uuid(), 
        v_store_id, 
        v_professional_3, 
        v_service_1, 
        'André Melo', 
        '+5511977777777', 
        'andre.melo@email.com',
        (CURRENT_DATE + INTERVAL '1 day' + TIME '14:30:00')::timestamp with time zone, 
        (CURRENT_DATE + INTERVAL '1 day' + TIME '15:00:00')::timestamp with time zone, 
        'confirmed', 
        'token-tomorrow-2', 
        false, 
        NOW()
    );
    
END $$;
