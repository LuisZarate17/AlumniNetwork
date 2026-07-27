-- CougarConnect demo seed data
--
-- Ten fictional alumni profiles so the live demo has something to search,
-- connect with, and message on first load. None of these are real WSU
-- alumni — names, companies, and emails are invented for this project
-- (see the disclaimer on the README screenshots). Run after schema.sql.
--
-- Explicit low ids (1-10) are used so they never collide with the random
-- 5-digit ids (10000-99999) real registrations get in Register.razor.

insert into "Alumni" ("id", "First", "Last", "GradYear", "City", "Major", "Company", "ConnectionList", "Email")
values
    (1, 'Ava', 'Thompson', '2020', 'Seattle', 'Computer Science', 'Nimbus Cloud Systems', '{}', 'ava.thompson@cougarconnect.demo'),
    (2, 'Marcus', 'Chen', '2019', 'Portland', 'Business Administration', 'Everbright Finance', '{}', 'marcus.chen@cougarconnect.demo'),
    (3, 'Priya', 'Patel', '2021', 'Spokane', 'Nursing', 'Harborview Health', '{}', 'priya.patel@cougarconnect.demo'),
    (4, 'Diego', 'Alvarez', '2018', 'Tacoma', 'Mechanical Engineering', 'Cascade Robotics', '{}', 'diego.alvarez@cougarconnect.demo'),
    (5, 'Hannah', 'Kim', '2022', 'Bellevue', 'Marketing', 'Orchard Design Co.', '{}', 'hannah.kim@cougarconnect.demo'),
    (6, 'Jordan', 'Reyes', '2017', 'Vancouver', 'Civil Engineering', 'GreenGrid Energy', '{}', 'jordan.reyes@cougarconnect.demo'),
    (7, 'Sofia', 'Nguyen', '2023', 'Spokane', 'Computer Science', 'student', '{}', 'sofia.nguyen@cougarconnect.demo'),
    (8, 'Ethan', 'Brooks', '2016', 'Seattle', 'Finance', 'Vantage Retail Group', '{}', 'ethan.brooks@cougarconnect.demo'),
    (9, 'Lena', 'Whitfield', '2020', 'Yakima', 'Education', 'Fieldstone Consulting', '{}', 'lena.whitfield@cougarconnect.demo'),
    (10, 'Owen', 'Marsh', '2019', 'Spokane', 'Computer Science', 'BluePeak Software', '{}', 'owen.marsh@cougarconnect.demo')
on conflict ("id") do nothing;

-- Keep the identity sequence ahead of the seeded ids so future inserts that rely on
-- the default (rather than the app's own random id) don't collide with these rows.
select setval(pg_get_serial_sequence('"Alumni"', 'id'), 11, false);
